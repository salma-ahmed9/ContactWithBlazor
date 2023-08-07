module default {
  type Contact {
    required username: str;
    required password: str;
    required role: str;
    required first_name: str; 
    required last_name: str; 
    required email: str; 
    required title: str; 
    required birth_date: datetime; 
    required description: str; 
    required marriage_status: bool; 
 }
}
