open System;
open System.IO;
open System.Xml.Serialization;

let toBytes (x : string) = System.Text.Encoding.ASCII.GetBytes x

let deserializeXml<'a> (xml : string) =
    let xmlSerializer = new XmlSerializer(typeof<'a>)
    use stream = new MemoryStream(toBytes xml)
    xmlSerializer.Deserialize stream :?> 'a

[<CLIMutable>]
[<XmlRoot("channel")>] 
type Channel = 
    {
        [<XmlElement("title")>]
        Title: string

        [<XmlElement("link")>]
        Link: string
    }

[<CLIMutable>]
[<XmlRootAttribute(ElementName="rss")>]
type rss = 
    { 
        [<XmlAttribute("version")>]
        version: string
    }

let reader = new StreamReader(@"redstavel.wordpress.2024-01-20.xml")
let comish = deserializeXml<rss> (reader.ReadToEnd())

()
