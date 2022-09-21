using System.Security.Principal;

namespace ASCTracInterfaceService.Filters
{
    /// <summary>
    /// Basic Authentication identity
    /// </summary>
    public class BasicAuthenticationIdentity : GenericIdentity
    {
        /// <summary>
        /// Get/Set for Extra Parameter
        /// </summary>
        public string Param { get; set; }
        /// <summary>
        /// Get/Set for Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Get/Set for UserId
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Basic Authentication Identity Constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="param"></param>
        public BasicAuthenticationIdentity(string token, string param)
            : base(token, "Basic")
        {
            Token = token;
            Param = param;
        }
    }
}