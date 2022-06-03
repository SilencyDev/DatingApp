namespace API.Controllers;

public class FallbackController  : Controller
{
    public ActionResult index() {
		return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
	}
}

