import go

from File f, PackageName pn 
where pn.getFile() = f
select f.getBaseName() as filename, pn.toString() as code
