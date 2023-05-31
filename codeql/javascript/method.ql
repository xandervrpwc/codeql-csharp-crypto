/**
 * @name query file and methodnames
 */

import javascript

from File file, Method method
where method.getFile() = file
select file.getBaseName() as filename, method.getName() as code
