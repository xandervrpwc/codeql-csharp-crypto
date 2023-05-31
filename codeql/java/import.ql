import java

from File f, Import imp
where imp.getFile() = f
select f.getBaseName() as filename, imp.getName() as code
