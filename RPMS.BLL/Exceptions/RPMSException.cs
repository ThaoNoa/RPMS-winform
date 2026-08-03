using System;

namespace RPMS.BLL.Exceptions
{
    public class RPMSException : Exception
    {
        public RPMSException(string message) : base(message) { }
    }

    public class NotFoundException : RPMSException
    {
        public NotFoundException(string entityName, object key)
            : base($"Không tìm thấy {entityName} với định danh ({key}).") { }
    }

    public class BadRequestException : RPMSException
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class UnauthorizedException : RPMSException
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}