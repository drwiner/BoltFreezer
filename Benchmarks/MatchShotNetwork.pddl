(define (domain unity-simple-domain)

    (:types   s-elmnt plan-elmnt cam-param integer - object
		step literal - plan-elmnt
		step-s step-d - step
		step-c - step-d
		operator-type - plan-elmnt
		movable-thing - s-elmnt
		place - s-elmnt
		person thing - movable-thing
		segment - object
		virtual-shot-type - object
		scale fov angle orient - cam-param
		)
		
	(:constants
		start mid end - segment
		low eye high - angle
		cu full wide - scale
		front behind-right - orient
		0 1 - integer
	)

    (:predicates

		;; Predicates associated with sub-plans 
		(has ?c - person ?t - thing)
        (at ?thing - movable-thing ?place - place)
        (= ?o1 ?o2 - object)
		(facing ?c1 ?c2 - person)
		(arg ?i - integer ?s - plan-elmnt ?o - object)
		(bel ?p - plan-elmnt)

		;; constraint-based predicates 
		(linked-by ?s ?t - step ?l - literal)
		(< ?s1 ?s2 - step)
		(has-scale ?s - step-c ?sc - scale)
		(has-angle ?s - step-c ?a - angle)
		(has-orient ?s - step ?ort - orient)
		(type ?s - step ?t - operator-type)
		(effect ?s - step ?l - literal)
		(precond ?s - step ?l - literal)
		(cntg ?d1 ?d2 - step-d)
	)
                    
  ;;  (:action strut
 ;;       :type step-s
  ;;      :parameters (?c - person ?p1 ?p2 - place)
 ;;       :precondition (and (at ?c ?p1) (not (= ?p1 ?p2)) )
 ;;       :effect(and (not (at ?c ?p1)) (at ?c ?p2))
 ;;   )


	;; when ground, decomposes into primitive tasks that acheive content, and 
	(:task cam-shot
	    :type step-c
		:camera (?scale - scale ?orient - orient ?angle - angle)
		:content (?step - s ?segm - segment)
	)


	(:task match-shot-pair
	    :type step-d
	    :parameters(?mshot1 ?mshot2)
	    :effect (and (bel ?state))
	    :decomp(
	        :fabula-params (?s - step-s)
	        :fabula-requirements(effect ?s ?state)
	        :discourse-params (?c - step-c)
	        :discourse-requirements(effect ?c (bel ?s))
	    )
	)
	
	(:method
		:head (match-shot-pair ?m1 ?m2)
		:precondition ()

	)


)