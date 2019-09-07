<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
   <xsl:output method="xml" version="1.0" encoding="iso-8859-1" omit-xml-declaration="yes" standalone="no" indent="yes" />

   <xsl:template match="/">
      <ROOT>
         <xsl:for-each select="ROOT/*[not(name(.)='STRING_CONNECTION'or name(.)='AUTH_DOMAINS_GROUPS' or name(.)='PATH_TEMPLATE') ]">
            <xsl:sort select="name()" />

            <xsl:call-template name="SERVICE" />
         </xsl:for-each>
      </ROOT>
   </xsl:template>

   <xsl:template name="SERVICE">
      <service>
         <endPoint>
            <xsl:value-of select="name()" />
         </endPoint>

         <verb>
            <xsl:choose>
               <xsl:when test="@VERB">
                  <xsl:value-of select="@VERB" />
               </xsl:when>

               <xsl:otherwise>POST</xsl:otherwise>
            </xsl:choose>
         </verb>
         <description>
            <xsl:value-of select="COMMENT" />
         </description>

         <data>
            <xsl:for-each select="PARAMETERS_LIST/PARAMETER">
               <xsl:element name="{text()}">
                  <xsl:value-of select="@TYPE" />

                  <xsl:if test="@TYPE">(optional)</xsl:if>
               </xsl:element>
            </xsl:for-each>
         </data>
      </service>
   </xsl:template>
</xsl:stylesheet>

