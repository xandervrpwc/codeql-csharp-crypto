/**
 * @name query methods
 */
 
import python

from File f, Function fun
where f.getExtension() = "py"
  and fun.inSource()
select f.getBaseName() as filename, fun.getName() as code
