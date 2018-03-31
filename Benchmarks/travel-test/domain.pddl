(define (domain travel-test)
    (:types  ticket person place transportation-device - thing
		plane car - transportation-device)
    (:predicates
        (person ?person - person)
        (place ?place - place)
        (at ?thing - thing ?place - thing)
        (in ?person - person ?vehicle - transportation-device)
        (has-ticket ?person - person)
        )
                    
    (:action get-in-car
        :parameters (?person - person ?car - car ?place - place)
        :precondition (and (at ?person ?place)
                           (at ?car ?place))
        :effect (and (in ?person ?car)
                    (not (at ?person ?place)))
	)
                    
    (:action drive
        :parameters (?person - person ?car - car ?from - place ?to - place)
        :precondition (and (at ?car ?from)
                        (not (= ?from ?to))
                           (in ?person ?car))
        :effect (and (at ?car ?to)
                    (not (at ?car ?from)))
	)
                    
    (:action get-out-of-car
        :parameters (?person - person ?car - car ?place - place)
        :precondition (and (at ?car ?place)
                           (in ?person ?car))
        :effect (and (at ?person ?place)
                    (not (in ?person ?car)))
	)
                    
    (:action buy-tickets
        :parameters (?person - person)
        :precondition ()
        :effect (has-ticket ?person)
	)
        
    (:action board-plane
        :parameters (?person - person ?plane - plane ?place - place)
        :precondition (and (at ?person ?place)
                           (at ?plane ?place)
                           (has-ticket ?person))
        :effect (and (in ?person ?plane)
                    (not (at ?person ?place))
                    (not (has-ticket ?person)))
	)
                    
    (:action fly
        :parameters (?person - person ?plane - plane ?from - place ?to - place)
        :precondition (and (at ?plane ?from) (not (= ?from ?to))
                            (in ?person ?plane))
        :effect (and (at ?plane ?to)
                   (not (at ?plane ?from)))
	)
                   
    (:action deplane
        :parameters (?person - person ?plane - plane ?place - place)
        :precondition (and (at ?plane ?place)
                           (in ?person ?plane))
        :effect (and (at ?person ?place)
                     (not (in ?person ?plane)))
	)

)