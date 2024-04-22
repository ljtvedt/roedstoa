// For more information see https://aka.ms/fsharp-console-apps
open System
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

let getItemCategories (itemMap: Map<int,WpModel.Item>) (itemId: int) = 
  match itemMap |> Map.tryFind itemId with
  | Some i -> i.categories |> toSwCategories
  | None -> [||]

let toSwTags (postmetas: (WpModel.PostMeta [])) =
    postmetas
    |> Array.map (fun x ->
        { Tag.key = x.meta_key
          value = x.meta_value })

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
            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              postedDate = DateTimeOffset.Parse(x.post_date_gmt + " +000")
              creator = x.creator
              wpUrl = x.attachment_url
              wpPostId = x.post_id
              wpParentPostId = x.post_parent
              wpPostName = x.post_name
              wpStatus = x.status
              wpPostType = x.post_type
              category = x.categories |> toSwCategories
              parentCategories = x.post_parent |> getItemCategories
              tags = x.postmetas |> toSwTags
              parentTags = [||] })

    let wpdmDocuments =
        items
        |> Array.filter (fun x -> x.status <> "draft")
        |> Array.filter (fun x -> x.post_type = "wpdmpro")
        |> Array.map (fun x ->
            { Dokument.title = x.title
              publicationDate = x.pubDate |> parsePublicationDate
              postedDate = DateTimeOffset.Parse(x.post_date_gmt + " +000")
              creator = x.creator
              wpUrl =
                x.postmetas
                |> Array.filter (fun x -> x.meta_key = "__wpdm_files")
                |> Array.map (fun x -> x.meta_value)
                |> Array.find (fun x -> true)
              wpPostId = x.post_id
              wpParentPostId = x.post_parent
              wpPostName = x.post_name
              wpStatus = x.status
              wpPostType = x.post_type
              category = x.categories |> toSwCategories
              parentCategories = x.post_parent |> getItemCategories
              tags = x.postmetas |> toSwTags
              parentTags = [||] })

    wpdmDocuments
    |> Array.filter (fun x -> x.title = "Årsmøtereferat 2019")
    |> Array.iter (fun x -> printfn $"{x}")

    attachmentDocuments |> Array.iter (fun x -> printfn $"{x}")

    // rss.channel.item
    // |> Array.filter (fun x -> x.post_type = "wpdmpro")
    // |> Array.iter (fun x -> printfn $"{x}")

    0
