/**
 * @name namespaces
 */

 import csharp

 from File f, Namespace n
 where f.fromSource() and n.getFile() = f
 select f.getBaseName() as filename, n.getName() as code
