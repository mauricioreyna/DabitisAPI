<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
   <xsl:key name="client-by-id" match="RECORD" use="SYSTEMID" />

   <xsl:output method="xml" version="1.0" encoding="iso-8859-1" omit-xml-declaration="no" standalone="no" indent="yes" />

   <xsl:template match="ROOT">
      <ROOT>
         <xsl:for-each select="//RECORD[count(. | key('client-by-id', SYSTEMID)[1]) = 1]">
            <DATACLIENT>
               <CLIENTID>
                  <xsl:value-of select="SYSTEMID" />
               </CLIENTID>

               <FIRSTNAME>
                  <xsl:value-of select="FIRSTNAME" />
               </FIRSTNAME>

               <LASTNAME>
                  <xsl:value-of select="LASTNAME" />
               </LASTNAME>

               <PRODUCTS>
                  <xsl:for-each select="key('client-by-id',SYSTEMID)">
                     <xsl:sort select="ID" order="ascending" />

                     <PRODUCT>
                        <ID>
                           <xsl:value-of select="ID" />
                        </ID>

                        <SUBPRODUCT>
                           <xsl:value-of select="SUBPRODUCT" />
                        </SUBPRODUCT>

                        <CURRENCY>
                           <xsl:value-of select="CURRENCY" />
                        </CURRENCY>

                        <LAST_PROCESS_DATE>
                           <xsl:value-of select="LAST_PROCESS_DATE" />
                        </LAST_PROCESS_DATE>

                        <BUK_NUMBER>
                           <xsl:value-of select="BUK_NUMBER" />
                        </BUK_NUMBER>

                        <BALANCE_TODAY>
                           <xsl:value-of select="BALANCE_TODAY" />
                        </BALANCE_TODAY>
                     </PRODUCT>
                  </xsl:for-each>
               </PRODUCTS>
            </DATACLIENT>
         </xsl:for-each>
      </ROOT>
   </xsl:template>
</xsl:stylesheet>
