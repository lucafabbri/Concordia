// Example of a list query for database management via REST API
using System.Collections.Generic;

namespace Concordia.Examples.Web.Controllers
{
    /// <summary>
    /// Query to retrieve all products from the database.
    /// </summary>
    /// <seealso cref="IRequest{List{ProductDto}}"/>
    public class ListProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
