<?xml version="1.0" encoding="Shift_JIS"?>
      <!--$reportdata.formdata.name -->
<formPrint name="$reportdata.formdata.name" printmode="$reportdata.formdata.printmode" cutflag="$reportdata.formdata.cutflag"  filename="$reportdata.formdata.filename" orientation="$reportdata.formdata.orientation">
#foreach ( $section in $reportdata.sections )
##如果section内没有row的定义则跳过
#if ( $section.rows.Count == 0 ) 

#else
       <!--$section.name -->
	<section name="$section.name" align="" fontsize="">
#foreach ( $row in $section.rows )
	      <!--$row.name -->
##如果行的定义为可选项△,那么追加条件判断语句
#if ( $row.condition == true ) 
#foreach ( $col in $row.cols )
#set ( $flag = $col.value )
#end
		\#if( ${flag}Flg == 1 )
#end
		<row align="center" fontsize="">
#foreach ( $col in $row.cols )
			      <!--$col.column -->
				<column align="$col.align" #if ( $col.fontsize == "" ) #else fontsize="$col.fontsize"  #end ul="" type="$col.type" value="$col.value" />
#end
		</row>
##如果行的定义为可选项△,那么追加条件判断语句
#if ( $row.condition == true ) 
		\#end
#end
#end
	</section>
#end
#end
</formPrint>

