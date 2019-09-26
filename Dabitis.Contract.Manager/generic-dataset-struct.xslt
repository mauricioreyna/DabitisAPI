<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method = "xml"  version="1.0" encoding="iso-8859-1" omit-xml-declaration="yes" standalone="no" indent="yes"  />
<xsl:template match="NewDataSet">
<TABLES>
                <!-- Obtengo las cantidad de ciclos-->

                <xsl:variable name="loopCount" select="number(translate(name(*[last()]),'oData',''))" />
                <xsl:attribute  name = "resultCode" >
                               <xsl:value-of select="//returnValue" />
                </xsl:attribute>
                <xsl:attribute  name = "service_name" >
                               <xsl:value-of select="//@servicename" />
                </xsl:attribute>
                <xsl:call-template name="for.loop">
                               <xsl:with-param name="i">0</xsl:with-param>
                               <xsl:with-param name="count"><xsl:value-of select="$loopCount" /></xsl:with-param>
                </xsl:call-template>
                <xsl:call-template name = "output" />
</TABLES>
</xsl:template>
<xsl:template name="output">
                <!--Conjunto de datos de output -->
                <xsl:if test = "//*[returnValue]/*[name()!='returnValue']">
                               <TABLE TYPE="OUTPUT">	
                               <RECORD>
                               <xsl:for-each select = "//*[returnValue]/*[name()!='returnValue']">
                                               <xsl:element  name = '{translate(name(), "abcdefghijklmnopqrstuvwxyz","ABCDEFGHIJKLMNOPQRSTUVWXYZ")}'><xsl:value-of select="." /></xsl:element>
                               </xsl:for-each>
                               </RECORD>
                               </TABLE>
                </xsl:if>
</xsl:template>
<!-- Use: include this call with the number of iterations
    This sample will loop from 1 to 10 including 1 and 10
-->
<!-- Rename "old name" elements to "new name" -->
<xsl:template name="for.loop">
                <xsl:param name="i" />
                <xsl:param name="count" />
                <xsl:if test="$i &lt;= $count">
                               <!--begin contenido -->
                               <xsl:variable name="nombretabla">oData<xsl:if test = "$i&gt;0"><xsl:value-of select="$i" /></xsl:if></xsl:variable>
                               <xsl:if test = "//*[name()=$nombretabla and not(returnValue)]">
                               <TABLE>
                                               <xsl:for-each select="//*[name()=$nombretabla]">
                                                               <RECORD>
                                                                              <xsl:for-each select = "*">
                                                                                              <xsl:variable name = "nombreNodo">
                                                                                                              <xsl:value-of select='translate(name(), "abcdefghijklmnopqrstuvwxyz","ABCDEFGHIJKLMNOPQRSTUVWXYZ")'/>
                                                                                              </xsl:variable>
                                                                                              <xsl:if test = "not($nombreNodo='RETURNVALUE')">
                                                                                                              <xsl:variable name = "nombreFinal">
                                                                                                              <xsl:choose>
                                                                                                                             <xsl:when test="starts-with($nombreNodo,'NULL')">COLUMN<xsl:value-of select="position()" /></xsl:when>
                                                                                                                             <xsl:otherwise><xsl:value-of select="$nombreNodo" /></xsl:otherwise>
                                                                                                              </xsl:choose>
                                                                                                              </xsl:variable>
                                                                                                              <xsl:element  name = "{$nombreFinal}">
                                                                                                                             <xsl:value-of select="." />
                                                                                                              </xsl:element>
                                                                                              </xsl:if>
                                                                              </xsl:for-each>
                                                               </RECORD>            
                                                               </xsl:for-each>
                               </TABLE>
                               </xsl:if>
                               <!--end contenido -->
      </xsl:if>
                <xsl:if test="$i &lt;= $count">
                               <xsl:call-template name="for.loop">
                                               <xsl:with-param name="i">
                                                               <!-- Increment index-->
                                                               <xsl:value-of select="$i + 1" />
                                               </xsl:with-param>
                                               <xsl:with-param name="count">
                                                               <xsl:value-of select="$count" />
                                               </xsl:with-param>
                               </xsl:call-template>
                </xsl:if>
                </xsl:template>
</xsl:stylesheet>
