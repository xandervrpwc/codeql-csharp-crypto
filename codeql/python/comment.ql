/**
 * @name Extracting Comments
 */

import python

from File f, CommentBlock c
where
  f.getExtension() = "py" and c.getContents() != ""
select f.getBaseName(), c.getContents()
