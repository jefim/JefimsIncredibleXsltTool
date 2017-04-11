README-SYNTAX.TXT

This document briefly explains how to use Jefim's Magical XSLT Syntax Concoctions.

1. Enable those by checking the checkbox in the bottom left corner of the UI.
2. This feature works two-way. So you can copy-paste an XSLT into the editor and then check the checkbox and the program will try to automatically convert your XSLT into a nicely readable one (== will add the syntax sugar).
3. If you do not like it - do not use it, that is why there is a checkbox :)

Here is a list of syntax sugar that is currently bundled:

------------------------------------------------------------------------------------
1. <xsl:value-of select="foobar" />					<=>		#echo foobar

------------------------------------------------------------------------------------
2. <xsl:param name="foobar" />						<=>		#param foobar

------------------------------------------------------------------------------------
3. <xsl:variable name="foo" value="bar" />			<=>		#var foo=bar

------------------------------------------------------------------------------------
4. Stylesheet starting and ending tags:

<xsl:stylesheet ...>								<=>		#here_be_xslt
	<xsl:output ...>
	...
</xsl:stylesheet>		

------------------------------------------------------------------------------------
5. Choose construct

<xsl:choose>										<=>		#choose
	<xsl:when test="$a = 123">						<=>			#when $a = 123
		...
	</xsl:when>
	<xsl:otherwise>									<=>			#else
		...
	</xsl:otherwise>								<=>			#end-else
</xsl:choose>										<=>		#end-choose



Here is an example with all above constructs used:

#here_be_xslt
	#param some_parameter
	<xsl:template match="/">
		#var a=1234
		<root>
			<hello>#echo $a</hello>
			<world>
			#choose
				#when $a = 123
					#echo 'Variable a is equal to 123'
				#when $a = 12
					#echo 'Variable a is equal to 12'
				#else
					#echo 'OTHERWISE Variable a is equal to '
					#echo $a
				#end-else
			#end-choose
			</world>
		</root>
	</xsl:template>