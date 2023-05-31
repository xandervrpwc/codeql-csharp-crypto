/**
 * @name class names
 */

 import csharp

 from File f, Class c
 where f.fromSource() and c.getFile() = f and f.getBaseName() in ["BCrypt.cs","BigInteger.cs","Kdf2BytesGenerator.cs","SCrypt.cs"]
 select f.getBaseName() as filename, c.getName() as code
