using Arzenal.Dto.DTOs.AppFileDto;
using Arzenal.Dto.DTOs.FileDto;
using Arzenal.Store.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArzenalStoreApi.Controllers.Apps
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy="ArzenalStoreManager")]
    public class AppFilesController : ControllerBase
    {
        private readonly IAppFilesService _appFilesService;

        public AppFilesController(IAppFilesService appFilesService)
        {
            _appFilesService = appFilesService;
        }

        /// <summary>
        /// Retourne le chemin d'accès du fichier associé à une application.
        /// </summary>
        /// <param name="appId">Identifiant unique de l'application.</param>
        /// <returns>
        /// Un objet contenant :  
        /// - <c>Id</c> : l'identifiant demandé  
        /// - <c>Filepath</c> : le chemin complet du fichier
        /// </returns>
        [HttpGet("filepath/{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFilePath(Guid appId)
        {
            var filepath = await _appFilesService.GetFilePath(appId);
            return Ok(new ReadAppFilePathDto { Id = appId, FilePath = filepath });
        }

        /// <summary>
        /// Récupère l'icône d'une application donnée.
        /// </summary>
        /// <param name="appId">Identifiant unique de l'application.</param>
        /// <returns>
        /// Le flux binaire de l'icône de l'application, avec le nom de fichier dans l'en-tête Content-Disposition.
        /// Renvoie <c>404 NotFound</c> si aucune icône n'est trouvée.
        /// </returns>
        [HttpGet("icon/{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[Produces("image/png")] // adapter selon le type d'image que tu utilises
        public async Task<IActionResult> GetAppIcon([FromRoute] Guid appId)
        {
            // Appel au service pour récupérer le flux de l'icône
            var result = await _appFilesService.GetAppIcon(appId);

            if (result == null || result.Stream == null)
                return NotFound();

            // Retourne le fichier avec le Content-Type correct et le nom du fichier
            return File(
                result.Stream,
                result.ContentType, // ex: "image/png" ou "image/jpeg"
                result.FileName     // ex: "icon.png"
            );
        }


        /// <summary>
        /// Télécharge le fichier de l'application correspondante.
        /// </summary>
        /// <param name="appId">Identifiant unique de l'application.</param>
        /// <returns>
        /// Le fichier binaire de l'application sous forme de flux,
        /// avec son nom réel dans l'en-tête <c>Content-Disposition</c>.
        /// </returns>
        [HttpGet("download/{appId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> DownloadApp(Guid appId)
        {
            var result = await _appFilesService.Download(appId);

            return File(
                result.Stream,
                result.ContentType,
                result.FileName
            );
        }


        /// <summary>
        /// Téléverse un fichier pour une application donnée.
        /// </summary>
        /// <param name="fileDto">
        /// Objet contenant les informations du fichier à téléverser, y compris :
        /// - <c>File</c> : le fichier envoyé dans un formulaire multipart (<c>multipart/form-data</c>),
        /// - <c>AppId</c> : l'identifiant unique de l'application,
        /// - <c>Version</c> : la version de l'application,
        /// - <c>Platform</c> : la plateforme cible (Android, Windows, iOS, etc.),
        /// - <c>Type</c> : le type de fichier (par exemple "app" ou "icon").
        /// </param>
        /// <returns>
        /// Une réponse <c>201 Created</c> contenant le chemin du fichier téléversé.
        /// Le corps de la réponse contient :
        /// - <c>Id</c> : l'identifiant de l'application,
        /// - <c>FilePath</c> : le chemin complet du fichier téléversé
        /// </returns>
        [HttpPost("upload/apps/{appId}/versions/{version}/files/{type}/filename/{fileName}/{platform?}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadFile(
            [FromRoute] Guid appId,
            [FromRoute] string version,
            [FromRoute] string type,
            [FromRoute] string fileName,
            [FromRoute] string? platform = null)
        {
            var dto = new UploadFileDto
            {
                AppId = appId,
                Version = version,
                Type = type,
                Platform = platform,
                FileName = fileName
            };

            var result = await _appFilesService.UploadFile(dto, Request.Body);
            return CreatedAtAction(
                nameof(GetFilePath),
                new { appId = dto.AppId },
                new ReadAppFilePathDto { Id = dto.AppId, FilePath = result }
            );
        }


        /// <summary>
        /// Supprime le fichier associé à une application donnée.
        /// </summary>
        /// <param name="id">
        /// Identifiant unique de l'application à laquelle le fichier doit être associé.
        /// </param>
        /// <returns>
        /// Une réponse <c>204 NoContent</c> en cas de succès.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            var result = await _appFilesService.DeleteFile(id);
            return NoContent();
        }
    }
}
