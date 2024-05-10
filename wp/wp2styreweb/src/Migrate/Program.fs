// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Text.RegularExpressions
open FSharp.Collections
open Migrate
open SWModel
open System.Globalization
open System.IO

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
    | None -> [||]

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


[<EntryPoint>]
let main args =
    let rss = WpModel.deserialize @"redstavel.wordpress.2024-01-20.xml"

    let items = rss.channel.item

    let itemMap =
        items
        |> Array.map (fun x -> (x.post_id, x))
        |> Map.ofArray

    let getItemCategories = getItemCategories itemMap

    let attachmentDocuments =
        items
        |> Array.filter (fun x -> x.status <> "draft")
        |> Array.filter (fun x -> x.post_type = "attachment")
        |> Array.map (fun x ->
            let categories = x.categories |> toSwCategories
            let parentCategories = x.post_parent |> getItemCategories
            let wpAttachedFile = x.postmetas |> getAttachedFile
            let newPath = newAttachedPath categories parentCategories wpAttachedFile

            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              postedDate = DateTimeOffset.Parse(x.post_date_gmt + " +000")
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
            let newPath = newWpdmPath categories parentCategories wpAttachedFile (x.post_date_gmt.Substring(0, 4))

            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              postedDate = DateTimeOffset.Parse(x.post_date_gmt + " +000")
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

    wpdmDocuments
    |> Array.iter (fun x -> printfn $"{x.title}\t{x.wpUrl}\t{x.newPath}")

// TODO: Lag ei liste basert på alle items, slik at vi kan lage ei komplett mapping
    // let usedCategories =
    //     wpdmDocuments
    //     |> Array.collect (fun x ->
    //         [x.categories; x.parentCategories]
    //         |> Array.concat)
    //     |> Array.distinct
    //     |> Array.sortBy (fun x -> x.name)

    // usedCategories
    // |> Array.iter (fun x ->
    //     printfn $"""{{domain = "{x.domain}"; nicename = "{x.nicename}"; swCategoryName=""; swCategoryPriority=""; swCategoryFilePath=""}}; """)

    // rss.channel.item
    // |> Array.filter (fun x -> x.post_type = "wpdmpro")
    // |> Array.iter (fun x -> printfn $"{x}")

    0
