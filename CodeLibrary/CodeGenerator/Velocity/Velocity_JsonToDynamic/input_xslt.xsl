﻿#foreach ( $ColumnInfo in $root.mappings.properties )
  $ColumnInfo.key
  $ColumnInfo.value.type
#end