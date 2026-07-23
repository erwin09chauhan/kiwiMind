namespace KiwiMind.Application.Common.Exceptions;

public class QuotaExceededException(string message) : Exception(message);
