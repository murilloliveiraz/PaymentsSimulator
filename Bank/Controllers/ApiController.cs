namespace Bank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        protected ActionResult CustomResponse(int status, bool success, object data = null)
        {
            return (status, success) switch
            {
                (404, false) => NotFound(new BaseResponse { StatusCode = status, Success = success, Message = "No elements found." }),
                (400, false) => NotFound(new BaseResponse { StatusCode = status, Success = success, Message = "Errors during the transation." }),
                (201, true) => Ok(new BaseResponse { StatusCode = status, Success = success, Message = "Created.", Data = data }),
                (200, true) => Ok(new BaseResponse { StatusCode = status, Success = success, Data = data })
            };
        }
    }
}
