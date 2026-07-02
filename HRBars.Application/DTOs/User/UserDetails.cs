using System;
using System.Collections.Generic;
using System.Text;

namespace HRBars.Application.DTOs.User
{
    public class UserDetails : UserResponse
    {
        public int CreatedApplicationsCount { get; set; }
        public int CreatedInterviewsCount { get; set; }
        public int DecidedInterviewsCount { get; set; }
        public int CommentsCount { get; set; }
        public DateTime? LastActiveAt { get; set; }
    }
}
