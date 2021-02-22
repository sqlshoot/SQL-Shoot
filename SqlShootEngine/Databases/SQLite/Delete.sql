DELETE FROM #tableName#
WHERE
    name LIKE '%#name#%' AND
    checksum LIKE '%#checksum#%' AND
    source LIKE '%#source#%' AND
    type LIKE '%#type#%' AND
    state LIKE '%#state#%'