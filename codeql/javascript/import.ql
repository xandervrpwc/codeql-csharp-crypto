/**
 * @name query imports
 */

import javascript

from File f, Import i
where i.getFile() = f
select f.getBaseName() as filename, i.toString() as code
