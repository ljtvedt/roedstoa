namespace Migrate

module SWModel =
    open System
    open System.IO
    open System.Xml.Serialization
    open FSharp.Collections

    type Tag = { key: string; value: string }

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
          content: string
          creator: string
          wpUrl: string
          wpPostId: int
          wpParentPostId: int
          wpPostName: string
          wpStatus: string
          wpPostType: string
          categories: CategoryMapping []
          parentCategories: CategoryMapping []
          attachedDocuments: Dokument array
          tags: Tag []
          parentTags: Tag [] }

    let pathOrganisasjon = "/Felles/Organisasjon"
    let pathInformasjonsskriv = "/Felles/Informasjonsskriv/{year}"
    let pathKomiteer = "/Felles/Komiteer"
    let pathVannOgAvløp = "/Felles/Vann- og avløpsprosjektet"
    let pathStyret = "/Felles/Styret/{year}"
    let pathVeikomiteen = "/Felles/Komiteer/Veikomiteen"
    let pathVannkomiteen = "/Felles/Komiteer/Vannkomiteen"
    let pathÅrsmøter = "/Felles/Årsmøter"
    let pathÅrsmøte2010 = "/Felles/Årsmøter/2010"
    let pathÅrsmøte2011 = "/Felles/Årsmøter/2011"
    let pathÅrsmøte2012 = "/Felles/Årsmøter/2012"
    let pathÅrsmøte2013 = "/Felles/Årsmøter/2013"
    let pathÅrsmøte2014 = "/Felles/Årsmøter/2014"
    let pathÅrsmøte2015 = "/Felles/Årsmøter/2015"
    let pathÅrsmøte2016 = "/Felles/Årsmøter/2016"
    let pathÅrsmøte2017 = "/Felles/Årsmøter/2017"
    let pathÅrsmøte2018 = "/Felles/Årsmøter/2018"
    let pathÅrsmøte2019 = "/Felles/Årsmøter/2019"
    let pathÅrsmøte2020 = "/Felles/Årsmøter/2020"
    let pathÅrsmøte2021 = "/Felles/Årsmøter/2021"
    let pathÅrsmøte2022 = "/Felles/Årsmøter/2022"
    let pathÅrsmøte2023 = "/Felles/Årsmøter/2023"
    let pathÅrsmøte2024 = "/Felles/Årsmøter/2024"


    let categoryMapList = [
      {domain = "wpdmcategory"; nicename = "formelt"; swCategoryName="Organisasjon"; swCategoryPriority = -1; swCategoryFilePath=pathOrganisasjon};
      {domain = "wpdmcategory"; nicename = "infoskriv"; swCategoryName="Informasjonsskriv"; swCategoryPriority=0; swCategoryFilePath=pathInformasjonsskriv};
      {domain = "post_tag"; nicename = "infoskriv"; swCategoryName="Informasjonsskriv"; swCategoryPriority=0; swCategoryFilePath=pathInformasjonsskriv};
      {domain = "wpdmcategory"; nicename = "komiteer"; swCategoryName="Komite"; swCategoryPriority=100; swCategoryFilePath=pathKomiteer};
      {domain = "post_tag"; nicename = "rsva"; swCategoryName="Vann- og avløpsprosjektet"; swCategoryPriority=5; swCategoryFilePath=pathVannOgAvløp};
      {domain = "wpdmcategory"; nicename = "styremote"; swCategoryName="Styremøte"; swCategoryPriority=2; swCategoryFilePath=pathStyret};
      {domain = "post_tag"; nicename = "styremote-referat"; swCategoryName="Styremøte"; swCategoryPriority=2; swCategoryFilePath=pathStyret};
      {domain = "post_tag"; nicename = "styret"; swCategoryName="Styret"; swCategoryPriority=2; swCategoryFilePath=pathStyret};
      {domain = "wpdmcategory"; nicename = "styret"; swCategoryName="Styret"; swCategoryPriority=2; swCategoryFilePath=pathStyret};
      {domain = "wpdmcategory"; nicename = "va-anlegg"; swCategoryName="Vann- og avløpsprosjektet"; swCategoryPriority=3; swCategoryFilePath=pathVannOgAvløp};
      {domain = "post_tag"; nicename = "vann-og-avlop"; swCategoryName="Vann- og avløpsprosjektet"; swCategoryPriority=3; swCategoryFilePath=pathVannOgAvløp};
      {domain = "wpdmcategory"; nicename = "vannkomiteen"; swCategoryName="Vannkomiteen"; swCategoryPriority=4; swCategoryFilePath=pathVannkomiteen};
      {domain = "wpdmcategory"; nicename = "veikomiteen"; swCategoryName="Veikomiteen"; swCategoryPriority=5; swCategoryFilePath=pathVeikomiteen};
      {domain = "post_tag"; nicename = "referat"; swCategoryName="Referat"; swCategoryPriority=90; swCategoryFilePath=pathStyret};
      {domain = "post_tag"; nicename = "vei"; swCategoryName="Veikomiteen"; swCategoryPriority=5; swCategoryFilePath=pathVeikomiteen};
      {domain = "wpdmcategory"; nicename = "arsmote"; swCategoryName="Årsmøter"; swCategoryPriority=80; swCategoryFilePath=pathÅrsmøter};
      {domain = "wpdmcategory"; nicename = "arsmote2010"; swCategoryName="Årsmøte 2010"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2010};
      {domain = "wpdmcategory"; nicename = "arsmote2011"; swCategoryName="Årsmøte 2011"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2011};
      {domain = "wpdmcategory"; nicename = "arsmote2012"; swCategoryName="Årsmøte 2012"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2012};
      {domain = "wpdmcategory"; nicename = "arsmote2013"; swCategoryName="Årsmøte 2013"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2013};
      {domain = "wpdmcategory"; nicename = "arsmote2014"; swCategoryName="Årsmøte 2014"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2014};
      {domain = "wpdmcategory"; nicename = "arsmote2015"; swCategoryName="Årsmøte 2015"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2015};
      {domain = "wpdmcategory"; nicename = "arsmote2016"; swCategoryName="Årsmøte 2016"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2016};
      {domain = "wpdmcategory"; nicename = "arsmote2017"; swCategoryName="Årsmøte 2017"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2017};
      {domain = "wpdmcategory"; nicename = "arsmote2018"; swCategoryName="Årsmøte 2018"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2018};
      {domain = "wpdmcategory"; nicename = "arsmote2019"; swCategoryName="Årsmøte 2019"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2019};
      {domain = "wpdmcategory"; nicename = "arsmote2020"; swCategoryName="Årsmøte 2020"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2020};
      {domain = "wpdmcategory"; nicename = "arsmote2021"; swCategoryName="Årsmøte 2021"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2021};
      {domain = "wpdmcategory"; nicename = "arsmote2022"; swCategoryName="Årsmøte 2022"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2022};
      {domain = "wpdmcategory"; nicename = "arsmote2023"; swCategoryName="Årsmøte 2023"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2023};
      {domain = "post_tag"; nicename = "arsmote-2023"; swCategoryName="Årsmøte 2023"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2023};
      {domain = "wpdmcategory"; nicename = "arsmote2023"; swCategoryName="Årsmøte 2023"; swCategoryPriority=1; swCategoryFilePath=pathÅrsmøte2023};
      {domain = "category"; nicename = "styret"; swCategoryName="Styret"; swCategoryPriority=2; swCategoryFilePath=pathStyret};
      {domain = "post_tag"; nicename = "info-fra-styret"; swCategoryName="Informasjonsskriv"; swCategoryPriority=0; swCategoryFilePath=pathInformasjonsskriv};
    ]

    let categoryMap  =
      categoryMapList
      |> List.map (fun x -> ((x.domain, x.nicename), x))
      |> Map.ofList
