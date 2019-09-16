##test1
#foreach ( $i in $list )
  $i
#end
   
   
##test2 
$user.TableName
#if($user.TableName == "user")
  hahaha
#else
 
#end
$user.Remark


##test3
${owner}:your  ${bill} type:${type} in  ${date} already paided  

##test4
$dict.username
$dict.age

##test5
$root.TableInfo.TableName
$root.TableInfo.Remark

#foreach ( $ColumnInfo in $root.TableInfo.Columns )
  $ColumnInfo.ColumnName
  $ColumnInfo.ColumnType
  $ColumnInfo.Age
         
  #foreach ( $schema in $ColumnInfo.Schemas )
    $schema
  #end  
#end