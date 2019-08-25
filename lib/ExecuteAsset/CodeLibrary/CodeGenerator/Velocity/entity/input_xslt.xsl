
 
-------------------------------entity--------------------------------- 
public class ${root.TableInfo.TableName}Entity { 
//$root.TableInfo.Remark

#foreach ( $ColumnInfo in $root.TableInfo.Columns )
#if($ColumnInfo.DataTypeName.equals("int"))
	#set($type = "Integer")
#elseif($ColumnInfo.DataTypeName.equals("tinyint"))
	#set($type = "Integer")
#elseif($ColumnInfo.DataTypeName.equals("datetime"))
	#set($type = "Timestamp")
#elseif($ColumnInfo.DataTypeName.equals("date"))
	#set($type = "Timestamp")
#else
	#set($type = "String")

#end
#if($ColumnInfo.IsKey.equals("*"))
@Id
#end
@Column(name = "$ColumnInfo.ColumnName") 
private $type $ColumnInfo.CameralColumnName;

#end



-------------------------------model--------------------------------- 

#foreach ( $ColumnInfo in $root.TableInfo.Columns )
#if($ColumnInfo.DataTypeName.equals("int"))
	#set($type = "Integer")
#elseif($ColumnInfo.DataTypeName.equals("tinyint"))
	#set($type = "Integer")
#elseif($ColumnInfo.DataTypeName.equals("datetime"))
	#set($type = "Long")
#elseif($ColumnInfo.DataTypeName.equals("date"))
	#set($type = "Long")
#else
	#set($type = "String")

#end
private $type $ColumnInfo.CameralColumnName;

#end

-------------------------------entity->model--------------------------------- 
return new ${root.TableInfo.TableName}Model(
#foreach ( $ColumnInfo in $root.TableInfo.Columns )
#if($ColumnInfo.DataTypeName.equals("int"))
	#set($type = "entity.get")
#elseif($ColumnInfo.DataTypeName.equals("tinyint"))
	#set($type = "entity.get")
	#set($endflag = "")
#elseif($ColumnInfo.DataTypeName.equals("datetime"))
	#set($type = "DateUtil.toUnixTime(")
	#set($endflag = ")")
#elseif($ColumnInfo.DataTypeName.equals("date"))
	#set($type = "DateUtil.toUnixTime(entity.get")
	#set($endflag = ")")
#else
	#set($type = "entity.get")
	#set($endflag = "")
#end
${type}${ColumnInfo.CameralColumnNameEx}()${endflag},
#end

