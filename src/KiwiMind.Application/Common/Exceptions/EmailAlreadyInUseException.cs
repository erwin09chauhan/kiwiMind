namespace KiwiMind.Application.Common.Exceptions;

public class EmailAlreadyInUseException(string email)
    : Exception($"An account with email '{email}' already exists.");
