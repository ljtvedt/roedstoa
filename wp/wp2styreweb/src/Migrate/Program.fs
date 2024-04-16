// For more information see https://aka.ms/fsharp-console-apps
open System
open FSharp.Collections
open Migrate

[<EntryPoint>]
let main args =
    let rss = WpModel.deserialize @"redstavel.wordpress.2024-01-20.xml"

    rss.channel.item
    |> Array.iter (fun x -> printfn $"{x}")

    0
