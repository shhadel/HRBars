namespace HRBars.Application.DTOs.Application;

public enum ApplicationStatus
{
    New = 1,
    InReview = 2,
    InterviewScheduled = 3,
    InterviewPassed = 4,
    InterviewFailed = 5,
    OfferExtended = 6,
    OfferAccepted = 7,
    OfferDeclined = 8,
    Hired = 9,
    Rejected = 10,
    Cancelled = 11
}