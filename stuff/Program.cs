using System.Text;
using System.Security.Cryptography;

var hashes = new HashSet<string>();

var start = new DateTime(1, 1, 1900);

while (true)
{

}

// var hashed = Hash(cpr);

string Hash(string cprNumber)
{
    var bytes = Encoding.UTF8.GetBytes(cprNumber);

    var hash = SHA256.HashData(bytes);

    return Convert.ToBase64String(hash);
}