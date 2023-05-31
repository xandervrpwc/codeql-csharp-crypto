 /**
 * @name method names
 */

 import csharp

 from File f, Method m
 where f.fromSource() and m.fromSource() and m.getFile() = f and f.getBaseName() in ["BCrypt.cs","BigInteger.cs","Kdf2BytesGenerator.cs","SCrypt.cs"]
 select f.getBaseName() as filename, m.getName() as code
