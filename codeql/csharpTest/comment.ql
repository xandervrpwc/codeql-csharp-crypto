/**
 * @name commentlines
 */

 import csharp

 from File f, CommentLine cl
 where f.fromSource() and f.getBaseName() in ["BCrypt.cs","BigInteger.cs","Kdf2BytesGenerator.cs","SCrypt.cs"]
 select f.getBaseName() as filename, cl.getText() as code
