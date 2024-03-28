<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
    xmlns:xd="http://www.oxygenxml.com/ns/doc/xsl"
    xmlns:emp="http://www.semanticalllc.com/ns/employees#"
    xmlns:h="http://www.w3.org/1999/xhtml"
    xmlns:fn="http://www.w3.org/2005/xpath-functions"
    xmlns:j="http://www.w3.org/2005/xpath-functions"
    xmlns:wp="http://wordpress.org/export/1.2/"
    exclude-result-prefixes="xs math xd h emp wp"
    version="1.0">
    
    <xsl:output encoding="UTF-8" indent="yes" />
    
    <!-- https://docstore.mik.ua/orelly/xml/xslt/ch06_02.htm -->
    <xsl:key name="post_type" match="item" use="wp:post_type" />
    
    <xsl:template match="rss/channel">
        <posts>
            <xsl:apply-templates select="item">
                <xsl:sort select="wp:post_type, wp:post_date_gmt"/>
            </xsl:apply-templates> 
        </posts>
    </xsl:template>
    
    <xsl:template match="item">
        <xsl:if test="wp:post_type='page' and wp:status!='draft'">
            <xsl:copy-of select="."/>
        </xsl:if>        
        <xsl:if test="wp:post_type='post' and wp:status!='draft'">
            <xsl:copy-of select="."/>
        </xsl:if>
        <xsl:if test="wp:post_type='wpdmpro' and wp:status!='draft'">
            <xsl:copy-of select="."/>
        </xsl:if>
        <xsl:if test="wp:post_type='attachment' and wp:status!='draft'">
            <xsl:copy-of select="."/>
        </xsl:if>
    </xsl:template>
    
</xsl:stylesheet>

<!-- Må handtere wpdmpro, lenker til desse, og lenker til andre downloads
     [wpdm_package, [wpdm_direct, [wpdm_category....]
     Korleis skal vi handtere kategoriar opp mot hierarkisk modell????
     
     Content kan innehalde lenker som er av type rel="attachment wp-att-805" og <img src class"wp-image-809"
-->

