namespace KiwiMind.Application.Common.Exceptions;

public class MessageTooLongException(string message) : Exception(message);
