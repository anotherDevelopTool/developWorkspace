
##test1
#foreach ( $i in $list )
    $i
   #end
   
   
  #test2 
 $user.TableName
 #if($user.TableName == "user")
 hahaha
 #else
 
 #end
 
 $user.Remark
 ##test3
 ${owner}:your  ${bill} type:${type} in  ${date} already paided  
 
 $tableinfo.TableName
 $tableinfo.Remark
 #foreach ( $ColumnInfo in $tableinfo.Columns )
     $ColumnInfo.ColumnName
         $ColumnInfo.ColumnType
             $ColumnInfo.Age
              #foreach ( $schema in $ColumnInfo.Schemas )
                   $schema
              #end
     
   #end
 