/**
 * @name query comments
 */

import javascript

from File f, Comment c
where c.getFile() = f and c.getText() != ""
select f.getBaseName() as filename, c.getText() as code
