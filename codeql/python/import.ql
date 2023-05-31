/**
 * @name Extracting importnames
 */
 
import python

from File f, Import imp
where f.getExtension() = "py" 
and f.fromSource()
and f.getBaseName() = "security.py"
select f.getBaseName() as filename, imp.toString() as code
