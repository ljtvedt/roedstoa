// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Text.RegularExpressions
open FSharp.Collections
open Migrate
open SWModel
open System.Globalization
open System.IO
open System.Text.RegularExpressions;

let parsePublicationDate (d: string) =
    let usCulture = CultureInfo("en-US");
    DateTimeOffset.ParseExact(d, "ddd, dd MMM yyyy HH:mm:ss zzzz", usCulture)

let toSwCategories (category: WpModel.Category []) =
    match category with
    | null -> [||]
    | _ ->
        category
        |> Array.choose (fun x -> categoryMap |> Map.tryFind (x.domain, x.nicename))
        |> Array.distinct

let getItemCategories (itemMap: Map<int, WpModel.Item>) (itemId: int) =
    match itemMap |> Map.tryFind itemId with
    | Some i -> i.categories |> toSwCategories
    | _ -> [||]

let getItemTitle (itemMap: Map<int, WpModel.Item>) (itemId: int) =
    match itemMap |> Map.tryFind itemId with
    | Some i -> i.title |> Some
    | None -> None

let toSwTags (postmetas: (WpModel.PostMeta [])) =
    postmetas
    |> Array.map (fun x ->
        { Tag.key = x.meta_key
          value = x.meta_value })

let getMetas (key: string) (postmetas: WpModel.PostMeta []) =
    postmetas
    |> Array.filter (fun x -> x.meta_key = key)
    |> Array.map (fun x -> x.meta_value)

let getMeta (key: string) (postmetas: WpModel.PostMeta []) =
    getMetas key postmetas
    |> Array.find (fun x -> true)

let getAttachedFile = getMeta "_wp_attached_file"

let getWpdmFiles postmetas =
    let fileString = getMeta "__wpdm_files" postmetas
    let extracted = Regex.Replace(fileString, "a:1:{i:\d+;s:\d+:\"*([^\";]*)\"*;}", "download-manager-files/$1")
    extracted.Replace("download-manager-files//home/roedsrcr/public_html/wp//wp-content/uploads", "")

let getPrioritizedPath (categories: SWModel.CategoryMapping array) =
    categories
    |> Array.sortBy (fun x -> x.swCategoryPriority)
    |> Array.map (fun x -> x.swCategoryFilePath)
    |> Array.tryHead

let newWpdmPath (categories: SWModel.CategoryMapping array) (parentCategories: SWModel.CategoryMapping array) (attachedFile: string) (year: string)=
    let filename = Path.GetFileName(attachedFile)
    match (getPrioritizedPath categories, getPrioritizedPath parentCategories) with
    | Some c, _ -> (sprintf $"{c}/{filename}").Replace ("{year}", year) |> Some
    | None, Some f -> (sprintf $"{f}/{filename}").Replace("{year}", year) |> Some
    | _ -> None

let newAttachedPath (categories: SWModel.CategoryMapping array) (parentCategories: SWModel.CategoryMapping array) (attachedFile: string) =
    let filename = Path.GetFileName attachedFile
    let monthPath = Path.GetDirectoryName attachedFile
    let yearPath = Path.GetDirectoryName monthPath
    let year = Path.GetFileName yearPath
    match (getPrioritizedPath categories, getPrioritizedPath parentCategories) with
    | Some c, _ -> (sprintf $"{c}/{filename}").Replace ("{year}", year) |> Some
    | None, Some f -> (sprintf $"{f}/{filename}").Replace("{year}", year) |> Some
    | _ -> None

let moveFile (source: string) (target: string) =
    Directory.CreateDirectory(Path.GetDirectoryName target) |> ignore
    try
        if File.Exists(source) then
            File.Move(source, target)
        else
            ()
    with exp -> printfn $"Duplikat filnamn {target}"

let moveDocument (sourceDirectory: string) (targetDirectory: string) (documents: Dokument array) =
    documents
    |> Array.map (fun x -> (x.wpAttachedFile, x.newPath))
    |> Array.map (fun (oldPath, newPath) -> (Path.Join(sourceDirectory, oldPath), newPath |> Option.map(fun x -> Path.Join(targetDirectory, x))))
    |> Array.iter (fun (oldPath, newPath) ->
        match (oldPath, newPath) with
        | (oP, Some nP) when oP.Length > 0 -> moveFile oP nP
        | _ -> printfn $"{oldPath} -> VERT IKKJE FLYTTA")

let getAuthorByLogin (authors: Map<string, string>) (email: string) =
    authors.TryFind email

let getDocumentById (documents: Dokument array) (id: int) =
    documents
    |> Array.filter (fun x -> x.wpPostId = id)
    |> Array.tryHead

let getDocumentByPath (documents: Dokument array) (path: string) =
    documents
    |> Array.filter (fun x -> x.wpUrl = path)
    |> Array.tryHead

let getAttachedDocuments (documents: Dokument array) (id: int) =
    documents
    |> Array.filter (fun x -> x.wpParentPostId = id)

let replace (source: string) (oldString: string) (newString: string) = source.Replace(oldString, newString)

let replaceRegexpMatch (getDocument: int -> Dokument option) (regexp: string) (documentIdGroup: int) (wholeMatchGroup: int) (content: string) =
    let matches = Regex.Matches(content, regexp, options = RegexOptions.Multiline)
    let commands =
        matches
        |> Seq.map (fun x ->
            let vedleggDokument =
                x.Groups[documentIdGroup].Value |> int |> getDocument |> Option.map (fun x -> x.title) |> Option.defaultValue "Ukjent"
            ($"{x.Groups[wholeMatchGroup].Value}", $"(se vedlegg '{vedleggDokument}')")
            )
    let documents =
        matches
        |> Seq.choose (fun x -> x.Groups[documentIdGroup].Value |> int |> getDocument)
        |> Array.ofSeq
    let newContent =
        Seq.fold( fun (acc: string) (oldString, newString) -> replace acc oldString newString ) content commands
    (newContent, documents)

let replaceRegexpMatchByPath (getDocument: string -> Dokument option) (regexp: string) (documentIdGroup: int) (wholeMatchGroup: int) (content: string) =
    let matches = Regex.Matches(content, regexp, options = RegexOptions.Multiline)
    let commands =
        matches
        |> Seq.map (fun x ->
            let vedleggDokument =
                x.Groups[documentIdGroup].Value |> getDocument |> Option.map (fun x -> x.title) |> Option.defaultValue "Ukjent"
            ($"{x.Groups[wholeMatchGroup].Value}", $"(se vedlegg '{vedleggDokument}')")
            )
    let documents =
        matches
        |> Seq.choose (fun x -> x.Groups[documentIdGroup].Value |> getDocument)
        |> Array.ofSeq
    let newContent =
        Seq.fold( fun (acc: string) (oldString, newString) -> replace acc oldString newString ) content commands
    (newContent, documents)


let replaceWpdmDirectLinks (getDocument: int -> Dokument option) (content: string) =
    replaceRegexpMatch getDocument @"(\[wpdm_direct_link\sid=(\d+) label=""([^""]*)""\])" 2 1 content

let replaceHrefsWithWpAttribute (getDocument: int -> Dokument option) (content: string) =
    replaceRegexpMatch getDocument @"(<a href=[^<]*wp-att-(\d*)[^<]*>([^<]*)<\/a>)" 2 1 content

let replaceHrefsAndImgsWithWpAttribute (getDocument: int -> Dokument option) (content: string) =
    replaceRegexpMatch getDocument @"(<a href=[^<]*wp-att-(\d*)""><img[^<]*>[^<]*<\/a>)" 2 1 content

let replaceSimpleHrefs (getDocument: string -> Dokument option) (content: string) =
    replaceRegexpMatchByPath getDocument @"(<a href=""([^""]*)"">([^<]*)<\/a>)" 2 1 content

let parseContent (getDocument: int -> Dokument option) (getDocumentByPath: string -> Dokument option) (content: string) : (string * Dokument array) =
    let r1 =  content |> replaceWpdmDirectLinks getDocument
    let r2 = fst r1 |> replaceHrefsWithWpAttribute getDocument
    let r3 = fst r2 |> replaceHrefsAndImgsWithWpAttribute getDocument
    let r4 = fst r3 |> replaceSimpleHrefs getDocumentByPath
    r4

let toPost getItemCategories getDocumentById getDocumentByPath getAttachedDocuments getAuthorByLogin (wpPost: WpModel.Item) =
    let categories = wpPost.categories |> toSwCategories
    let parentCategories = wpPost.post_id |> getItemCategories
    let attachedDocument = getAttachedDocuments wpPost.post_id
    let contentAndLinks =
        parseContent getDocumentById getDocumentByPath wpPost.content_encoded
    let creatorName = getAuthorByLogin wpPost.creator |> Option.defaultValue "Ukjent"
    let publicationDate = wpPost.pubDate |> parsePublicationDate
    let dateFormatInTitle = "d. MMMM yyyy"
    let dateFormatShort = "dd.MM.yyyy"
    let noCulture = CultureInfo("nb-NO");
    let convertHeading = $"<p align=\"right\"><i>Opprinnelig publisert {publicationDate.ToString(dateFormatInTitle, noCulture)} av {creatorName}</i></p>\n"
    let content = "<p>" + ((fst contentAndLinks).Replace("\n", "</p>\n<p>")) + "</p>"

    {
        Post.title = $"{wpPost.title} - ({publicationDate.ToString(dateFormatInTitle, noCulture)})"
        publicationDate = publicationDate
        publicationDateString = $"{publicationDate.ToString(dateFormatShort, noCulture)}"
        content = convertHeading + content
        creator = wpPost.creator
        creatorName = creatorName
        wpUrl = wpPost.attachment_url
        wpPostId = wpPost.post_id
        wpParentPostId = wpPost.post_parent
        wpPostName = wpPost.post_name
        wpStatus = wpPost.status
        wpPostType = wpPost.post_type
        categories = categories
        parentCategories = parentCategories
        attachedDocuments = [|attachedDocument; snd (contentAndLinks)|] |> Array.collect id
        tags = wpPost.postmetas |> toSwTags
        parentTags = [||] }


[<EntryPoint>]
let main args =
    let rss = WpModel.deserialize @"c:\Users\n638510\Privat\git\roedstoa\wp\wp2styreweb\redstavel.wordpress.2024-05-13.xml"

    let items = rss.channel.item

    let authors = rss.channel.author

    let itemMap =
        items
        |> Array.filter (fun x -> x.status <> "draft")
        |> Array.map (fun x -> (x.post_id, x))
        |> Map.ofArray

    let authorMap =
        authors
        |> Array.map (fun x -> (x.author_login, x.author_display_name))
        |> Map.ofArray

    let getItemCategories = getItemCategories itemMap

    let posts =
        items
        |> Array.filter (fun x -> x.status <> "draft")
        |> Array.filter (fun x -> x.post_type = "post")

    let attachmentDocuments =
        items
        |> Array.filter (fun x -> x.status <> "draft")
        |> Array.filter (fun x -> x.post_type = "attachment")
        |> Array.filter (fun x -> itemMap |> Map.containsKey x.post_parent)
        |> Array.map (fun x ->
            let categories = x.categories |> toSwCategories
            let parentCategories = x.post_parent |> getItemCategories
            let wpAttachedFile = x.postmetas |> getAttachedFile
            let newPath = newAttachedPath categories parentCategories wpAttachedFile

            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              creator = x.creator
              wpUrl = x.attachment_url
              wpAttachedFile = wpAttachedFile
              newPath = newPath
              wpPostId = x.post_id
              wpParentPostId = x.post_parent
              wpPostName = x.post_name
              wpStatus = x.status
              wpPostType = x.post_type
              categories = categories
              parentCategories = parentCategories
              tags = x.postmetas |> toSwTags
              parentTags = [||] })

    let wpdmDocuments =
        items
        |> Array.filter (fun x -> x.status <> "draft")
        |> Array.filter (fun x -> x.post_type = "wpdmpro")
        |> Array.map (fun x ->
            let categories = x.categories |> toSwCategories
            let parentCategories = x.post_parent |> getItemCategories
            let wpAttachedFile = x.postmetas |> getWpdmFiles
            let newPath = newWpdmPath categories parentCategories wpAttachedFile (x.post_date.Substring(0, 4))

            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              creator = x.creator
              wpUrl = x.postmetas |> getWpdmFiles
              wpAttachedFile = wpAttachedFile
              newPath = newPath
              wpPostId = x.post_id
              wpParentPostId = x.post_parent
              wpPostName = x.post_name
              wpStatus = x.status
              wpPostType = x.post_type
              categories = categories
              parentCategories = parentCategories
              tags = x.postmetas |> toSwTags
              parentTags = [||] })

    let allDocuments = [| wpdmDocuments; attachmentDocuments |] |> Array.collect id

    // Bygg ny dokumentstruktur
    // let sourceDirectory = """C:\Users\n638510\Privat\git\roedstoa\wp\wp2styreweb\roedstoaUploads"""
    // let targetDirectory = """C:\Users\n638510\Privat\git\roedstoa\wp\wp2styreweb\newPath"""
    // allDocuments |> moveDocument sourceDirectory targetDirectory

    let getDocumentById = getDocumentById allDocuments
    let getDocumentByPath = getDocumentByPath allDocuments
    let getAttachedDocuments = getAttachedDocuments allDocuments
    let getAuthorByLogin = getAuthorByLogin authorMap
    let toPost = toPost getItemCategories getDocumentById getDocumentByPath getAttachedDocuments getAuthorByLogin

    let swPosts =
        posts
        |> Array.sortBy (fun x -> x.post_date)
        |> Array.map toPost


    let sw = File.CreateText("posts.txt")
    swPosts
    // |> Array.filter (fun x -> x.attachedDocuments.Length > 0)
    // |> Array.truncate 1
    |> Array.iter (fun x ->
        let attached =
            x.attachedDocuments
            |> Array.map (fun x -> x.newPath |> Option.defaultValue "")
            |> Array.distinct
            |> Array.fold (fun s x  -> s + "\n" + x) ""
        let categories =
            x.categories
            |> Array.sortBy (fun x -> x.swCategoryPriority)
            |> Array.map (fun x -> x.swCategoryName)
            |> Array.fold (fun s x -> s + ", " + x) ""
        sw.WriteLine $"\n\n\n\n\n--TITTEL--\n{x.title}\n\n--PUBLISERING--\n{x.publicationDateString}\n{x.creatorName}\n\n--KATEGORIER--\n{categories}\n\n--INNHALD--\n\n{x.content}\n\n--VEDLEGG--\n{attached}")
    sw.Flush()

    // allDocuments
    // |> Array.iter (fun x -> printfn $"{x.title}\t{x.wpUrl}\t{x.newPath}")

    // Mangler path (og dermed sannsynlegvis kategoriar)
    // allDocuments
    // |> Array.filter (fun x -> Option. isNone x.newPath)
    // |> Array.iter (fun x -> printfn $"ParentTitle: {x.wpParentPostId |> getItemTitle itemMap }/{x.wpParentPostId}\t Title: {x.title}/{x.wpPostId}\t{x.wpUrl}\t{x.newPath}")

    //  Skriv ut alle kategoriar vi har brukt
    // let usedCategories =
    //     allDocuments
    //     |> Array.collect (fun x ->
    //         [x.categories; x.parentCategories]
    //         |> Array.concat)
    //     |> Array.distinct
    //     |> Array.sortBy (fun x -> sprintf "{x.domain}#{x.nicename}")
    // usedCategories
    // |> Array.iter (fun x ->
    //     printfn $"""{{domain = "{x.domain}"; nicename = "{x.nicename}"; swCategoryName=""; swCategoryPriority=""; swCategoryFilePath=""}}; """)

    // rss.channel.item
    // |> Array.filter (fun x -> x.post_type = "wpdmpro")
    // |> Array.iter (fun x -> printfn $"{x}")

    0
