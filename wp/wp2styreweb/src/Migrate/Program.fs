// For more information see https://aka.ms/fsharp-console-apps
open System
open System.Text.RegularExpressions
open FSharp.Collections
open Migrate
open SWModel

let parsePublicationDate (d: string) =
    DateTimeOffset.ParseExact(d, "ddd, dd MMM yyyy HH:mm:ss zzzz", null)

let toSwCategories (category: WpModel.Category []) =
    match category with
    | null -> [||]
    | _ ->
        category
        |> Array.map (fun x ->
            { Category.domain = x.domain
              nicename = x.nicename
              name = x.name })

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

let newPath (categories: SWModel.Category array) (parentCategories: SWModel.Category array) (attachedFile: string) =
    ""
    
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
            let newPath = newPath categories parentCategories wpAttachedFile

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
            let newPath = newPath categories parentCategories wpAttachedFile

            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              postedDate = DateTimeOffset.Parse(x.post_date_gmt + " +000")
              creator = x.creator
              wpUrl = x.postmetas |> getWpdmFiles
              wpAttachedFile = wpAttachedFile
              newPath = ""
              wpPostId = x.post_id
              wpParentPostId = x.post_parent
              wpPostName = x.post_name
              wpStatus = x.status
              wpPostType = x.post_type
              categories = categories
              parentCategories = parentCategories
              tags = x.postmetas |> toSwTags
              parentTags = [||] })

    // wpdmDocuments
    // |> Array.filter (fun x -> x.title = "Årsmøtereferat 2019")
    // |> Array.iter (fun x -> printfn $"{x}")

// TODO: Lag ei liste basert på alle items, slik at vi kan lage ei komplett mapping
    let usedCategories =
        wpdmDocuments
        |> Array.collect (fun x -> 
            [x.categories; x.parentCategories]
            |> Array.concat)
        |> Array.distinct
        |> Array.sortBy (fun x -> x.name)
    
    usedCategories |> Array.iter (fun x -> printfn $"{x.nicename} : {x.domain} : {x.name}")


    // rss.channel.item
    // |> Array.filter (fun x -> x.post_type = "wpdmpro")
    // |> Array.iter (fun x -> printfn $"{x}")

    0
