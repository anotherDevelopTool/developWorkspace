<?xml version="1.0" encoding="utf-8"?>
 <xsl:stylesheet version="1.0" 
                      xmlns:xsl="http://www.w3.org/1999/XSL/Transform"    
                      xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                      exclude-result-prefixes="msxsl">
     <xsl:output method="html" indent="yes"/>

     <xsl:template match="/">
          <table class="mainTable">
              <tr style="background:#f5f5f5;">
                  <th style="width:20%;">书名</th>
                  <th style="width:20%;">作者</th>
                  <th style="width:20%;">出版社</th>
                  <th style="width:20%;">出版日期</th>
                  <th style="width:20%;">定价</th>
              </tr>
              <xsl:for-each select="/BookStore/Book">
                  <xsl:element name="tr">
                      <xsl:element name="td">
                          <xsl:value-of select="Name" />
                      </xsl:element>
                      <xsl:element name="td">
                          <xsl:value-of select="Author" />
                      </xsl:element>
                      <xsl:element name="td">
                          <xsl:value-of select="Publisher" />
                      </xsl:element>
                      <xsl:element name="td">
                          <xsl:value-of 
                      select="msxsl:format-date(PubDate, 'yyyy-M-dd')" />
                      </xsl:element>
                      <xsl:element name="td">
                          <xsl:value-of select="Price" />
                      </xsl:element>
                  </xsl:element>             
              </xsl:for-each>
          </table> 
     </xsl:template>
 </xsl:stylesheet>