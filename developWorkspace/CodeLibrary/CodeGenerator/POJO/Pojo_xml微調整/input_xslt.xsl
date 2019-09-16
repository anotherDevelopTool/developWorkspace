<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:template match="/">
public class <xsl:value-of select="Table/@TableName"/>Dto extends CommonDto {
		<xsl:apply-templates select="Table/Column"/>
}
</xsl:template> 

<xsl:template match="Table/Column">
	/** <xsl:value-of select="Remark"/> */
	<xsl:choose>
          <xsl:when test="DataType = 'System.String'">
          private  String <xsl:value-of select="ColumnName"/>;
          </xsl:when>
          <xsl:when test="DataType = 'System.DateTime'">
          private  Date <xsl:value-of select="ColumnName"/>;
          </xsl:when>
          <xsl:when test="DataType = 'System.Int32'">
          private Integer <xsl:value-of select="ColumnName"/>;
          </xsl:when>
          <xsl:otherwise>
          </xsl:otherwise>
        </xsl:choose> 
</xsl:template> 

</xsl:stylesheet>
