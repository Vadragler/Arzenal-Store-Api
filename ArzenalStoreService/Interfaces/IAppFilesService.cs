using Arzenal.Dto.DTOs.AppFileDto;
using Arzenal.Dto.DTOs.FileDto;
using Microsoft.AspNetCore.Http;

namespace Arzenal.Store.Api.Service.Interfaces
{
    public record AppFileStreamResult(
    Stream Stream,
    string FileName,
    string ContentType
    );
    public interface IAppFilesService
    {
        public Task<string> GetFilePath(Guid appId);
        public Task<FileResultDto> GetAppIcon(Guid appId);
        public Task<AppFileStreamResult> Download(Guid appId);
        public Task<string> UploadFile(UploadFileDto fileDto, Stream input);
        public Task<bool> DeleteFile(Guid appId);
    }
}
