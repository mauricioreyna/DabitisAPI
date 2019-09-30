<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
   <xsl:output method="xml" version="1.0" encoding="iso-8859-1" omit-xml-declaration="yes" standalone="no" indent="yes" />

   <xsl:template match="/">
      <ROOT>
         <TABLES>
            <xsl:for-each select="ROOT/TABLES/@*">
               <xsl:copy />
            </xsl:for-each>

            <TABLE>
               <RECORD>
                  <SYSCOMMENT>
                     <xsl:apply-templates select="//RECORD" />
                  </SYSCOMMENT>
               </RECORD>
            </TABLE>
         </TABLES>
      </ROOT>
   </xsl:template>

   <xsl:template match="RECORD">
      <xsl:apply-templates select="TEXT" />
   </xsl:template>

   <xsl:template match="TEXT">
      <xsl:value-of select="." />
   </xsl:template>
</xsl:stylesheet>

