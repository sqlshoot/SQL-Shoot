DELETE FROM "#schemaName#"."#tableName#"
WHERE
    name = '#name#' AND
    checksum = '#checksum#' AND
    source = '#source#' AND
    type = '#type#' AND
    state = '#state#';