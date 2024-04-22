namespace Migrate

module WpModel =
    open System
    open System.IO
    open System.Xml.Serialization
    open FSharp.Collections

    [<Literal>]
    let xmlns_excerpt = "http://wordpress.org/export/1.2/excerpt/"

    [<Literal>]
    let xmlns_content = "http://purl.org/rss/1.0/modules/content/"

    [<Literal>]
    let xmlns_wfw = "http://wellformedweb.org/CommentAPI/"

    [<Literal>]
    let xmlns_dc = "http://purl.org/dc/elements/1.1/"

    [<Literal>]
    let xmlns_wp = "http://wordpress.org/export/1.2/"

    [<CLIMutable>]
    [<XmlRoot("postmeta", Namespace = xmlns_wp)>]
    type PostMeta =
        { [<XmlElement("meta_key", Namespace = xmlns_wp)>]
          meta_key: string

          [<XmlElement("meta_value", Namespace = xmlns_wp)>]
          meta_value: string }

    [<CLIMutable>]
    [<XmlRootAttribute(ElementName = "category")>]
    type Category =
        { [<XmlAttribute("domain")>]
          domain: string

          [<XmlAttribute("nicename")>]
          nicename: string

          [<XmlText>]
          name: string }

    [<CLIMutable>]
    [<XmlRoot("item")>]
    type Item =
        { [<XmlElement("title")>]
          title: string

          [<XmlElement("link")>]
          link: string

          [<XmlElement("pubDate")>]
          pubDate: string

          [<XmlElement("creator", Namespace = xmlns_dc)>]
          creator: string

          [<XmlElement("guid")>]
          guid: string

          [<XmlElement("description")>]
          description: string

          [<XmlElement("encoded", Namespace = xmlns_content)>]
          content_encoded: string

          [<XmlElement("encoded", Namespace = xmlns_excerpt)>]
          excerpt_encoded: string

          [<XmlElement("post_id", Namespace = xmlns_wp)>]
          post_id: int

          [<XmlElement("post_date", Namespace = xmlns_wp)>]
          post_date: string

          [<XmlElement("post_date_gmt", Namespace = xmlns_wp)>]
          post_date_gmt: string

          [<XmlElement("comment_status", Namespace = xmlns_wp)>]
          comment_status: string

          [<XmlElement("ping_status", Namespace = xmlns_wp)>]
          ping_status: string

          [<XmlElement("post_name", Namespace = xmlns_wp)>]
          post_name: string

          [<XmlElement("status", Namespace = xmlns_wp)>]
          status: string

          [<XmlElement("post_parent", Namespace = xmlns_wp)>]
          post_parent: int

          [<XmlElement("menu_order", Namespace = xmlns_wp)>]
          menu_order: int

          [<XmlElement("post_type", Namespace = xmlns_wp)>]
          post_type: string

          [<XmlElement("is_sticky", Namespace = xmlns_wp)>]
          is_sticky: int

          [<XmlElement("category")>]
          categories: Category []

          [<XmlElement("attachment_url", Namespace = xmlns_wp)>]
          attachment_url: string

          [<XmlElement("postmeta", Namespace = xmlns_wp)>]
          postmetas: PostMeta [] }

    [<CLIMutable>]
    [<XmlRoot("channel")>]
    type Channel =
        { [<XmlElement("title")>]
          title: string

          [<XmlElement("link")>]
          link: string

          [<XmlElement("item")>]
          item: Item [] }

    [<CLIMutable>]
    [<XmlRootAttribute(ElementName = "rss")>]
    type rss =
        { [<XmlAttribute("version")>]
          version: string

          [<XmlElement("channel")>]
          channel: Channel }

    let deserialize filename =
        let serializer = XmlSerializer(typeof<rss>)
        use reader = new FileStream(filename, FileMode.Open)
        serializer.Deserialize(reader) :?> rss
