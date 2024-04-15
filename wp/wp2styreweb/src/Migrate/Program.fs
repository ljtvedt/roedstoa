// For more information see https://aka.ms/fsharp-console-apps
open System
open System.IO
open System.Xml.Serialization
open FSharp.Collections

[<CLIMutable>]
[<XmlRoot("item")>] 
type Item = {
        [<XmlElement("title")>]
        title: string
        [<XmlElement("pubDate")>]
        pubDate:string
        [<XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")>]
        creator: string
        [<XmlElement("post_type", Namespace = "http://wordpress.org/export/1.2/")>]
        post_type: string
        [<XmlElement("encoded", Namespace = "http://purl.org/rss/1.0/modules/content/")>]
        content_encoded: string
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
    let serializer = XmlSerializer(typeof<rss>);
    use reader = new FileStream(@"redstavel.wordpress.2024-01-20.xml", FileMode.Open)
    let rss = serializer.Deserialize(reader) :?> rss;

    printfn $"{rss.version}"

    rss.channel.item
    |> Array.iter (fun x -> printfn $"{x.title}: {x.post_type}: {x.creator}: {x.content_encoded}")

    0
