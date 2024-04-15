// For more information see https://aka.ms/fsharp-console-apps
open System
open System.IO
open System.Xml.Serialization
open FSharp.Collections

let toBytes (x : string) = System.Text.Encoding.ASCII.GetBytes x

let deserializeXml<'a> (xml : string) =
    let xmlSerializer = new XmlSerializer(typeof<'a>)
    use stream = new MemoryStream(toBytes xml)
    xmlSerializer.Deserialize stream :?> 'a

[<CLIMutable>]
[<XmlRoot("item")>] 
type Item = {
        [<XmlElement("title")>]
        title: string
        [<XmlElement("post_type")>]
        post_type: string
}

[<CLIMutable>]
[<XmlRoot("channel")>] 
type Channel = 
    {
        [<XmlElement("title")>]
        title: string

        [<XmlElement("link")>]
        link: string

        [<XmlElement>]
        item: Item []
    }

[<CLIMutable>]
[<XmlRootAttribute(ElementName="rss")>]
type rss = 
    { 
        [<XmlAttribute("version")>]
        version: string

        [<XmlElement("channel")>]
        channel: Channel
    }


[<EntryPoint>]
let main args =
    let reader = new StreamReader(@"redstavel.wordpress.2024-01-20.xml")
    let rss = deserializeXml<rss> (reader.ReadToEnd())
    printfn $"{rss.version}"

    rss.channel.item
    |> Array.iter (fun x -> printfn $"{x.title}")

    0
