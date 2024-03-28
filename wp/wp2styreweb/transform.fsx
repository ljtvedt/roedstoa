open System.Xml.Xsl

let xslt = XslCompiledTransform()

// https://learn.microsoft.com/en-us/dotnet/api/system.xml.xsl.xslcompiledtransform?view=net-8.0
// https://www.w3schools.com/xml/xsl_intro.asp
xslt.Load("wordpress2styreweb.xslt");
xslt.Transform("redstavel.wordpress.2024-01-20.xml", "styreweb.xml");