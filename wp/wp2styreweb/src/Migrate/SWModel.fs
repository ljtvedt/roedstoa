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

    type CategoryMapping = {
      domain: string
      nicename: string
      swCategoryName: string
      swCategoryPriority: int
      swCategoryFilePath: string
    }

    type Dokument =
        { title: string
          publicationDate: DateTimeOffset
          postedDate: DateTimeOffset
          creator: string
          wpUrl: string
          wpAttachedFile: string
          newPath: string option
          wpPostId: int
          wpParentPostId: int
          wpPostName: string
          wpStatus: string
          wpPostType: string
          categories: CategoryMapping []
          parentCategories: CategoryMapping []
          tags: Tag []
          parentTags: Tag [] }

    type Post =
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
          category: CategoryMapping []
          parentCategories: CategoryMapping []
          tags: Tag []
          parentTags: Tag [] }


    let categoryMapList = [
      {domain = "wpdmcategory"; nicename = "formelt"; swCategoryName="Organisasjon"; swCategoryPriority=0; swCategoryFilePath="/Felles/Organisasjon"};
      {domain = "wpdmcategory"; nicename = "infoskriv"; swCategoryName="Informasjonsskriv"; swCategoryPriority=10; swCategoryFilePath="/Felles/Informasjonsskriv"};
      {domain = "post_tag"; nicename = "infoskriv"; swCategoryName="Informasjonsskriv"; swCategoryPriority=10; swCategoryFilePath="/Felles/Informasjonsskriv"};
      {domain = "wpdmcategory"; nicename = "komiteer"; swCategoryName="Komite"; swCategoryPriority=100; swCategoryFilePath="/Felles/Komiteer"};
      {domain = "post_tag"; nicename = "rsva"; swCategoryName="Vann- og avløpsprosjektet"; swCategoryPriority=5; swCategoryFilePath="/Felles/Vann- og avløpsprosjektet"};
      {domain = "wpdmcategory"; nicename = "styremote"; swCategoryName="Styremøte"; swCategoryPriority=2; swCategoryFilePath="/Felles/Styret/{year}"};
      {domain = "post_tag"; nicename = "styremote-referat"; swCategoryName="Styremøte"; swCategoryPriority=2; swCategoryFilePath="/Felles/Styret/{year}"};
      {domain = "post_tag"; nicename = "styret"; swCategoryName="Styret"; swCategoryPriority=2; swCategoryFilePath="/Felles/Styret/{year}"};
      {domain = "wpdmcategory"; nicename = "styret"; swCategoryName="Styret"; swCategoryPriority=2; swCategoryFilePath="/Felles/Styret/{year}"};
      {domain = "wpdmcategory"; nicename = "va-anlegg"; swCategoryName="Vann- og avløpsprosjektet"; swCategoryPriority=3; swCategoryFilePath="/Felles/Vann- og avløpsprosjektet"};
      {domain = "post_tag"; nicename = "vann-og-avlop"; swCategoryName="Vann- og avløpsprosjektet"; swCategoryPriority=3; swCategoryFilePath="/Felles/Vann- og avløpsprosjektet"};
      {domain = "wpdmcategory"; nicename = "vannkomiteen"; swCategoryName="Vannkomiteen"; swCategoryPriority=4; swCategoryFilePath="/Felles/Komiteer/Vannkomiteen"};
      {domain = "wpdmcategory"; nicename = "veikomiteen"; swCategoryName="Veikomiteen"; swCategoryPriority=5; swCategoryFilePath="/Felles/Komiteer/Veikomiteen"};
      {domain = "post_tag"; nicename = "referat"; swCategoryName="Referat"; swCategoryPriority=90; swCategoryFilePath="/Felles/Styret/{year}"};
      {domain = "post_tag"; nicename = "vei"; swCategoryName="Veikomiteen"; swCategoryPriority=5; swCategoryFilePath="/Felles/Komiteer/Veikomiteen"};
      {domain = "wpdmcategory"; nicename = "arsmote"; swCategoryName="Årsmøter"; swCategoryPriority=80; swCategoryFilePath="/Felles/Årsmøter"};
      {domain = "wpdmcategory"; nicename = "arsmote2010"; swCategoryName="Årsmøte 2010"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2010"};
      {domain = "wpdmcategory"; nicename = "arsmote2011"; swCategoryName="Årsmøte 2011"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2011"};
      {domain = "wpdmcategory"; nicename = "arsmote2012"; swCategoryName="Årsmøte 2012"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2012"};
      {domain = "wpdmcategory"; nicename = "arsmote2013"; swCategoryName="Årsmøte 2013"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2013"};
      {domain = "wpdmcategory"; nicename = "arsmote2014"; swCategoryName="Årsmøte 2014"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2014"};
      {domain = "wpdmcategory"; nicename = "arsmote2015"; swCategoryName="Årsmøte 2015"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2015"};
      {domain = "wpdmcategory"; nicename = "arsmote2016"; swCategoryName="Årsmøte 2016"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2016"};
      {domain = "wpdmcategory"; nicename = "arsmote2017"; swCategoryName="Årsmøte 2017"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2017"};
      {domain = "wpdmcategory"; nicename = "arsmote2018"; swCategoryName="Årsmøte 2018"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2018"};
      {domain = "wpdmcategory"; nicename = "arsmote2019"; swCategoryName="Årsmøte 2019"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2019"};
      {domain = "wpdmcategory"; nicename = "arsmote2020"; swCategoryName="Årsmøte 2020"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2020"};
      {domain = "wpdmcategory"; nicename = "arsmote2021"; swCategoryName="Årsmøte 2021"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2021"};
      {domain = "wpdmcategory"; nicename = "arsmote2022"; swCategoryName="Årsmøte 2022"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2022"};
      {domain = "wpdmcategory"; nicename = "arsmote2023"; swCategoryName="Årsmøte 2023"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2023"};
      {domain = "post_tag"; nicename = "arsmote-2023"; swCategoryName="Årsmøte 2023"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2023"};
      {domain = "wpdmcategory"; nicename = "arsmote2023"; swCategoryName="Årsmøte 2023"; swCategoryPriority=1; swCategoryFilePath="/Felles/Årsmøter/Årsmøte 2023"};
    ]

    let categoryMap  =
      categoryMapList
      |> List.map (fun x -> ((x.domain, x.nicename), x))
      |> Map.ofList
