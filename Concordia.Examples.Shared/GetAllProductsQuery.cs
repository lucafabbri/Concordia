// Example Controller for usage (remains unchanged)
using System.Collections.Generic;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// The get all products query class
    /// </summary>
    /// <seealso cref="IRequest{List{ProductDto}}"/>
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
