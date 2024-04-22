namespace Migrate

module SWModel =
    open System
    open System.IO
    open System.Xml.Serialization
    open FSharp.Collections

    type Tag = { key: string; value: string }

    type Category =
        { domain: string
          nicename: string
          name: string }

    type Dokument =
        { title: string
          publicationDate: DateTimeOffset
          postedDate: DateTimeOffset
          creator: string
          wpUrl: string
          wpPostId: int
          wpParentPostId: int
          wpPostName: string
          wpStatus: string
          wpPostType: string
          category: Category []
          parentCategories: Category []
          tags: Tag []
          parentTags: Tag [] }
